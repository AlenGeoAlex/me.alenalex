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
    if(opts.bucket === 'PUT')
        return undefined;

    const r2Url = `https://${opts.accountId}.r2.cloudflarestorage.com`;

    const client = new AwsClient({
        service: "s3",
        region: "auto",
        accessKeyId: opts.accessKeyId,
        secretAccessKey: opts.secretAccessKey,
    });
    opts.validityInSeconds = opts.validityInSeconds || 60;
    let finalPath = `${r2Url}/${opts.bucket}/${opts.path}?X-Amz-Expires=${opts.validityInSeconds}`;

    const request = new Request(finalPath, {
        method: opts.method,
    })

    const signedRequest = await client.sign(request, {
        method: opts.method,
        aws: {
            signQuery: true
        }
    });

    if(signedRequest.url)
        return signedRequest.url.toString();

    return undefined;
}