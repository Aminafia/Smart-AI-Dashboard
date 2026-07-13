import { Injectable } from '@angular/core';

import { marked } from 'marked';
import DOMPurify from 'dompurify';

@Injectable({
  providedIn: 'root'
})
export class MarkdownService {

  render(markdown: string): string {

    const html = marked.parse(markdown) as string;

    return DOMPurify.sanitize(html);

  }

}