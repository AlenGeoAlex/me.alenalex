import {AppContext} from "../types";

/**
 * Utility class for extracting client IP addresses from requests
 */
export class IPUtils {
    /**
     * Get the client IP address from a Hono context
     * @param c - Hono context
     * @returns IP address or 'unknown' if not found
     */
    static getClientIP(c: any): string {
        const cfConnectingIP = c.req.header('CF-Connecting-IP');
        if (cfConnectingIP) {
            return cfConnectingIP;
        }

        // Method 2: From Cloudflare context object
        const cfIP = c.req.raw?.cf?.connectingIp;
        if (cfIP) {
            return cfIP;
        }

        // Method 3: X-Real-IP header (common reverse proxy header)
        const xRealIP = c.req.header('X-Real-IP');
        if (xRealIP) {
            return xRealIP;
        }

        // Method 4: X-Forwarded-For header (take the first IP in the chain)
        const xForwardedFor = c.req.header('X-Forwarded-For');
        if (xForwardedFor) {
            const firstIP = xForwardedFor.split(',')[0].trim();
            if (firstIP) {
                return firstIP;
            }
        }

        // Method 5: True-Client-IP header (Akamai, Cloudflare Enterprise)
        const trueClientIP = c.req.header('True-Client-IP');
        if (trueClientIP) {
            return trueClientIP;
        }

        // Fallback: unknown
        return 'unknown';
    }

    /**
     * Validate if a string is a valid IPv4 address
     * @param ip - IP address string to validate
     * @returns true if valid IPv4
     */
    static isValidIPv4(ip: string): boolean {
        const ipv4Pattern = /^(\d{1,3}\.){3}\d{1,3}$/;
        if (!ipv4Pattern.test(ip)) {
            return false;
        }

        const parts = ip.split('.');
        return parts.every(part => {
            const num = parseInt(part, 10);
            return num >= 0 && num <= 255;
        });
    }

    /**
     * Validate if a string is a valid IPv6 address (basic check)
     * @param ip - IP address string to validate
     * @returns true if valid IPv6
     */
    static isValidIPv6(ip: string): boolean {
        const ipv6Pattern = /^([\da-f]{1,4}:){7}[\da-f]{1,4}$/i;
        const ipv6CompressedPattern = /^([\da-f]{1,4}:)*::([\da-f]{1,4}:)*[\da-f]{1,4}$/i;
        return ipv6Pattern.test(ip) || ipv6CompressedPattern.test(ip);
    }

    /**
     * Validate if a string is a valid IP address (IPv4 or IPv6)
     * @param ip - IP address string to validate
     * @returns true if valid IP
     */
    static isValidIP(ip: string): boolean {
        return this.isValidIPv4(ip) || this.isValidIPv6(ip);
    }

    /**
     * Get the client IP and validate it
     * @param c - Hono context
     * @returns Valid IP address or null if invalid/unknown
     */
    static getValidatedClientIP(c: any): string | null {
        const ip = this.getClientIP(c);
        if (ip === 'unknown' || !this.isValidIP(ip)) {
            return null;
        }
        return ip;
    }
}