import { Injectable } from '@angular/core';

type HashInput = File | Blob | ArrayBuffer | Uint8Array;

@Injectable({
  providedIn: 'root',
})
export class HashService {
  async hash(
    input: HashInput,
    algorithm: 'SHA-1' | 'SHA-256' | 'SHA-384' | 'SHA-512' = 'SHA-256'
  ): Promise<string> {
    const buffer = await this.toArrayBuffer(input);
    const hashBuffer = await crypto.subtle.digest(algorithm, buffer);
    return this.arrayBufferToHex(hashBuffer);
  }

  async hashText(
    text: string,
    algorithm: 'SHA-1' | 'SHA-256' | 'SHA-384' | 'SHA-512' = 'SHA-256'
  ): Promise<string> {
    const encoder = new TextEncoder();
    return this.hash(encoder.encode(text), algorithm);
  }

  private async toArrayBuffer(input: HashInput): Promise<ArrayBuffer> {
    if (input instanceof File || input instanceof Blob) {
      return await input.arrayBuffer();
    }

    if (input instanceof Uint8Array) {
      return input.slice().buffer;
    }

    return input;
  }

  private arrayBufferToHex(buffer: ArrayBuffer): string {
    const bytes = new Uint8Array(buffer);
    return Array.from(bytes)
      .map((b) => b.toString(16).padStart(2, '0'))
      .join('');
  }
}
