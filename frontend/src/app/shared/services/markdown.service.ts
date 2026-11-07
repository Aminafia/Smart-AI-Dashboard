import { Injectable } from '@angular/core';

import DOMPurify from 'dompurify';
import { marked } from 'marked';

@Injectable({
  providedIn: 'root',
})
export class MarkdownService {
  render(markdown: string): string {
    const html = marked.parse(markdown) as string;

    return DOMPurify.sanitize(html);
  }
}
