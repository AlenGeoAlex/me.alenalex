import {OpenAPIRoute} from "chanfana";
import {z} from "zod";
import {AppContext} from "../../types";

export class NowPlaying extends OpenAPIRoute {

    schema = {
        tags: ["Me"],
        summary: "Get the current now playing song",
        responses: {
            "200": {
                description: "Returns the current now playing song",
                content: {
                    "application/json": {
                        schema: z.object({
                            title: z.string(),
                            artist: z.string(),
                            itemImage: z.string().nullable(),
                            album: z.string().nullable(),
                            albumArt: z.string().nullable(),
                            duration: z.number(),
                            totalDuration: z.number(),
                            deviceType: z.string(),
                            trackType: z.enum(["track", "episode", "ad", "unknown"]),
                        }),
                    }
                }
            },
            "204": {
                description: "No song is currently playing",
            },
            "401": {
                description: "No spotify credentials provided",
            }
        }
    }

    private static readonly SPOTIFY_API_URL = "https://api.spotify.com/v1/me/player";


    async handle(c: AppContext) {
        if (c.env.SPOTIFY_REFRESH_TOKEN === undefined)
            return c.json({error: "No spotify credentials provided"}, 401);

        let accessToken = await c.env.NowPlayingStore.get('SPOTIFY_ACCESS_TOKEN', "text");
        if (!accessToken) {
            console.log('No access token found, generating new one');
            const credentials = await this.generateCredentials(c);
            if (credentials instanceof Error) {
                return c.json({error: "Failed to generate credentials due to [" + credentials.message + "]"}, 500);
            }

            accessToken = credentials.accessToken;
            await c.env.NowPlayingStore.put('SPOTIFY_ACCESS_TOKEN', accessToken, {expirationTtl: credentials.expiresIn - (60 * 10)});
            await c.env.NowPlayingStore.put('SPOTIFY_REFRESH_TOKEN', credentials.refreshToken);
        }

        const currentPlaying = await this.getNowPlaying(c, accessToken);

        const responseHeaders = new Headers();

        if (currentPlaying instanceof Error) {
            console.error('Failed to get now playing: ' + currentPlaying.message);
            responseHeaders.set('Cache-Control', 'public, max-age=120');
            return c.json({error: currentPlaying.message}, 500);
        }

        if (currentPlaying === undefined) {
            responseHeaders.set('Cache-Control', 'public, max-age=30');
            return new Response(null, {status: 204, headers: responseHeaders});
        }

        const remainingSeconds = Math.floor((currentPlaying.totalDuration - currentPlaying.duration) / 1000);
        responseHeaders.set('Cache-Control', `public, max-age=${remainingSeconds}`);
        return new Response(JSON.stringify(currentPlaying), {
            status: 200,
            headers: responseHeaders,
        });
    }

    async getNowPlaying(
        c: AppContext,
        accessToken: string,
    ) : Promise<{
        title: string,
        artist: string,
        album?: string,
        albumArt?: string,
        duration: number,
        totalDuration: number,
        deviceType: string
        trackType: "track" | "episode" | "ad" | "unknown",
        itemImage?: string,
    } | undefined | Error> {
        const url = new URL(NowPlaying.SPOTIFY_API_URL);

        const nowPlayingResponse = await fetch(url, {
            headers: {
                'Authorization': `Bearer ${accessToken}`,
                'content-type': 'application/json',
            }
        });

        if(!nowPlayingResponse.ok){
            console.error('Failed to get spotify now playing');
            const error = await nowPlayingResponse.text();
            return new Error(error);
        }

        if (nowPlayingResponse.status === 204) {
            return undefined;
        }

        const nowPlayingData : any = await nowPlayingResponse.json();
        return {
            deviceType: nowPlayingData.device.type,
            trackType: nowPlayingData.currently_playing_type,
            title: nowPlayingData.item.name,
            artist: nowPlayingData.currently_playing_type === 'track' ?
                nowPlayingData.item.artists?.map((artist: any) => artist.name).join(', ') ?? 'N/A' :
                nowPlayingData.item.show.name,
            album: nowPlayingData.item.album.name,
            albumArt: nowPlayingData.item.album.images[0].url,
            duration: nowPlayingData.progress_ms,
            totalDuration: nowPlayingData.item.duration_ms,
            itemImage: nowPlayingData.item.album.images.length > 0 ? nowPlayingData.item.album.images[0].url : undefined,
        }
    }

    async generateCredentials(
        c: AppContext,
    ) : Promise<{
        accessToken: string,
        refreshToken: string,
        expiresIn: number,
    } | Error> {
        let refreshToken = await c.env.NowPlayingStore.get('SPOTIFY_REFRESH_TOKEN', "text")
        if(!refreshToken || refreshToken === "undefined"){
            console.log('No refresh token found, Trying to populate from env');
            refreshToken = c.env.SPOTIFY_REFRESH_TOKEN;
        }

        // Even after trying to take from env
        if(!refreshToken){
            return new Error('No refresh token found');
        }

        const url = new URL("https://accounts.spotify.com/api/token");
        const refreshResponse = await fetch(url, {
            headers: {
                'content-type': 'application/x-www-form-urlencoded',
                'Authorization': `Basic ${btoa(`${c.env.SPOTIFY_CLIENT_ID}:${c.env.SPOTIFY_CLIENT_SECRET}`)}`,
            },
            method: 'POST',
            body: new URLSearchParams({
                grant_type: 'refresh_token',
                refresh_token: refreshToken,
            }),
        })

        if(!refreshResponse.ok){
            console.error('Failed to refresh spotify access token');
            const error = await refreshResponse.text();
            return new Error(error);
        }

        const refreshData : any = await refreshResponse.json();

        return {
            accessToken: refreshData.access_token,
            refreshToken: refreshData.refresh_token,
            expiresIn: refreshData.expires_in,
        }
    }
}