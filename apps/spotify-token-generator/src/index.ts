import { execSync } from "child_process";
// @ts-ignore
import input from 'input';
import * as process from "node:process";

/**
 * The main entry point for the application. Prompts the user for their Spotify client ID,
 * client secret, and redirect URI to generate an authorization URL, attempts to open it
 * in the browser, and processes the authorization code for obtaining credentials.
 *
 * @return {Promise<void>} A promise that resolves when the process completes successfully
 * or terminates if required inputs are invalid or browser opening fails.
 */
async function main() {
    const clientId: string = (await input.text('Please enter your spotify client id:')).trim();
    if (!clientId) {
        console.error('Client id is required');
        process.exit(1);
    }

    const clientSecret: string = (await input.text('Please enter your spotify client secret:')).trim();
    if (!clientSecret) {
        console.error('Client secret is required');
        process.exit(1);
    }

    let redirectUri: string = (await input.text('Please enter your spotify redirect uri you configured in https://developer.spotify.com/dashboard/ (default: https://127.0.0.1/):')).trim();
    if (!redirectUri) {
        console.log('No redirect uri provided, using default');
        redirectUri = 'https://127.0.0.1/';
    }

    const authorizationUrl = await constructUrlForAuthorization(clientId, redirectUri);


    try {
        execSync(`open "${authorizationUrl.toString()}"`)
    }catch (e) {
        console.log('Failed to open browser, please open the following url manually:');
        console.log(authorizationUrl.toString());
    }

    const codeOutput = await awaitAndParseCode();
    console.log('Matched code output, exchanging for credentials...');

    const token = await requestToken(codeOutput.code, clientId, clientSecret, redirectUri)
    if (token instanceof Error) {
        console.error('Failed to retrieve token:', token.message);
        process.exit(1)
    }

    console.log('The token has been successfully retrieved:');
    console.log(`   AccessToken: ${token.access_token}`);
    console.log(`   Refresh: ${token.refresh_token}`);
    console.log(`   Expires in: ${token.expires_in}`)
    process.exit(0)
}

/**
 * Constructs a URL for initiating the authorization process with Spotify.
 *
 * @param {string} clientId - The client identifier for the Spotify application.
 * @param {string} redirectUri - The URI to redirect to after authorization is complete.
 * @return {Promise<string>} A promise that resolves to the constructed authorization URL.
 */
async function constructUrlForAuthorization(clientId: string, redirectUri: string): Promise<string> {
    const authorizationUrl = new URL('https://accounts.spotify.com/authorize');
    authorizationUrl.searchParams.append('client_id', clientId);
    authorizationUrl.searchParams.append('response_type', 'code');
    authorizationUrl.searchParams.append('redirect_uri', redirectUri);
    authorizationUrl.searchParams.append('scope', 'user-read-currently-playing user-read-email');
    return authorizationUrl.toString();
}

/**
 * Prompts the user to input a code or redirect URI, processes the input, and extracts the authorization code.
 * If the input is a redirect URI containing a code, the code is extracted and returned. If the input is
 * provided directly as a code, it is returned as is. If no code or redirect URI is provided, the process exits.
 *
 * @return {Promise<{code: string, state?: string}>} A promise that resolves to an object containing the code and state. If a state is not available in the input, it will not be included in the returned object.
 */
async function awaitAndParseCode() : Promise<{
    code: string
    state?: string
}>{
    const passedInput = (await input.text('Please enter the code or redirect uri:')).trim();
    if (!passedInput) {
        console.error('No code or redirect uri provided');
        process.exit(1);
    }

    if(passedInput.startsWith("http")){
        const url = new URL(passedInput);
        const code = url.searchParams.get('code')?.trim();
        const state = url.searchParams.get('state')?.trim();
        if(code){
            return {code, state};
        }

        console.error('No code found in redirect uri');
        process.exit(1)
    }

    return passedInput;
}

async function requestToken(code: string, clientId: string, clientSecret: string, redirectUri: string): Promise<{
    access_token: string,
    refresh_token: string,
    expires_in: number,
    token_type: 'Bearer',
} | Error> {
    const url = new URL('https://accounts.spotify.com/api/token');
    const response = await fetch(url, {
        headers: {
            'content-type': 'application/x-www-form-urlencoded',
            'Authorization': `Basic ${Buffer.from(`${clientId}:${clientSecret}`).toString('base64')}`
        },
        method: 'POST',
        body: new URLSearchParams({
            grant_type: 'authorization_code',
            code,
            redirect_uri: redirectUri,
        }),
    });

    if (!response.ok) {
        return response.text().then(text => new Error(text));
    }

    const responseJson = await response.json();
    return  {
        access_token: responseJson.access_token,
        refresh_token: responseJson.refresh_token,
        expires_in: responseJson.expires_in,
        token_type: responseJson.token_type,
    }
}

await main();
