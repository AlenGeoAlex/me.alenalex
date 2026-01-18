import {OpenAPIRoute, Str} from "chanfana";
import {z} from "zod";
import {AppContext} from "../../types";
import {IPUtils} from "../../utils/ip-utils";
import {createMimeMessage, Mailbox} from "mimetext/browser";
import {EmailMessage} from "cloudflare:email";

export class ContactMe extends OpenAPIRoute {
    schema = {
        tags: ["Me"],
        summary: "Contact me",
        request: {
            body: {
                content: {
                    "application/json": {
                        schema: z.object({
                            email: Str({
                                description: "Email to send the message to",
                                required: true
                            }),
                            message: Str({
                                description: "Message to send",
                                required: true
                            }),
                            captchaKey: Str({
                                description: "Captcha key",
                                required: true
                            })
                        })
                    }
                }
            }
        },
        responses: {
            "200": {
                description: "Success - message sent",
                content: {
                    "application/json": {
                        schema: z.object({success: z.boolean()})
                    }
                }
            },
            "400": {
                description: "Bad Request - Invalid captcha key",
                content: {
                    "application/json": {
                        schema: z.object({error: z.string()})
                    }
                }
            }
        }
    };

    async handle(c: AppContext) {
        const data = await this.getValidatedData<typeof this.schema>();
        if(!data.body)
            return c.json({
                error: 'Failed to parse request body'
            }, 400);

        const ip = IPUtils.getValidatedClientIP(c)
        const validityResponse = await this.validateCaptcha(data.body.captchaKey, c.env.TURNSTILE_SECRET_KEY, ip);
        if(!validityResponse.success)
            return c.json({
                error: 'Invalid captcha key'
            });

        return await this.sendEmail(data.body.email, data.body.message, c);
    }

    async sendEmail(email: string, message: string, c : AppContext) {
        const msg = createMimeMessage();
        msg.setSender({
            type: 'From',
            name: 'Contact Mailbox',
            addr: email
        });
        msg.setHeader('Reply-To', new Mailbox(email))
        msg.setRecipient(c.env.DESTINATION_ADDRESS);
        msg.setSubject(`[me.alenalex] [PORTFOLIO] New message from ${email}]`);
        msg.addMessage({
            contentType: 'text/plain',
            data: message
        });

        const emailMessage = new EmailMessage(
            email,
            c.env.DESTINATION_ADDRESS,
            msg.asRaw()
        );

        try{
            await c.env.EMAIL_BINDING.send(emailMessage);
            return c.json({success: true});
        }catch (e) {
            console.error(e);
            return c.json({
                error: 'Failed to send email'
            }, 400);
        }
    }

    private async validateCaptcha(captchaKey: string, secretKey: string, ip : string | undefined, maxRetry : number = 3) : Promise<{
        success: boolean,
        "error-codes": string[]
    }> {
        const idKey = crypto.randomUUID();
        for (let attempt = 1; attempt <= maxRetry; attempt++) {
            try {
                const formData = new FormData();
                formData.append("secret", secretKey);
                formData.append("response", captchaKey);
                formData.append("remoteip", ip);
                formData.append("idempotency_key", idKey);

                const response = await fetch(
                    "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                    {
                        method: "POST",
                        body: formData,
                    },
                );

                const result = await response.json();

                if (response.ok) {
                    return {
                        success: true,
                        "error-codes": [],
                    }
                }

                if (attempt === maxRetry) {
                    return {
                        success: false,
                        "error-codes": ["internal-error", 'attempt-reached-max'],
                    }
                }

                await new Promise((resolve) =>
                    setTimeout(resolve, Math.pow(2, attempt) * 200),
                );
            } catch (e) {
                console.error(e);
                if (attempt === maxRetry) {
                    return { success: false, "error-codes": ["internal-error"] };
                }
            }
        }
    }
}