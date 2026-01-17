import {AwsClient} from "aws4fetch";

export async function getSignedUrl(opts: {
    accessKeyId: string,
    secretAccessKey: string,
    bucket: string,
    path: string
    accountId: string
    method?: 'GET' | 'PUT',
    validityInSeconds?: number,
}) : Promise<string | undefined> {
    opts.method = opts.method || 'GET';
    if(opts.method === 'PUT')
        return undefined;

    const r2Url = `https://${opts.accountId}.r2.cloudflarestorage.com`;

    const client = new AwsClient({
        service: "s3",
        region: "auto",
        accessKeyId: opts.accessKeyId,
        secretAccessKey: opts.secretAccessKey,
    });

    opts.validityInSeconds = opts.validityInSeconds || 60;

    let finalPath = `${r2Url}/${opts.bucket}/${opts.path}`;

    const request = new Request(finalPath, {
        method: opts.method,
    });

    const signedRequest = await client.sign(request, {
        method: opts.method,
        aws: {
            signQuery: true,
            datetime: new Date().toISOString().replace(/[:-]|\.\d{3}/g, ''),
        }
    });

    if(signedRequest.url) {
        let signedUrl = signedRequest.url.toString();
        if (!signedUrl.includes('X-Amz-Expires')) {
            const separator = signedUrl.includes('?') ? '&' : '?';
            signedUrl += `${separator}X-Amz-Expires=${opts.validityInSeconds}`;
        }
        return signedUrl;
    }

    return undefined;
}