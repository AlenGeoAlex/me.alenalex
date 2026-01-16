// import {OpenAPIRoute} from "chanfana";
// import {AppContext} from "../../types";
// import {AwsClient} from "aws4fetch";
//
// export class CvGet extends OpenAPIRoute {
//     schema: {
//         "200": {
//             "application/json": string
//         }
//     }
//
//     handle(c: AppContext) {
//         const accountId = c.env.R2_ACCOUNT_ID;
//         const r2Url = `https://${accountId}.r2.cloudflarestorage.com`;
//
//         const client = new AwsClient({
//             service: "s3",
//             region: "auto",
//             accessKeyId: c.env.R2_ACCESS_KEY,
//             secretAccessKey: c.env.R2_SECRET_KEY,
//         });
//
//         const path
//     }
// }