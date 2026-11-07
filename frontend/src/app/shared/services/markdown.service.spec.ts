import { TestBed } from '@angular/core/testing';

import { MarkdownService } from './markdown.service';

describe('MarkdownService', () => {
  let service: MarkdownService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MarkdownService);
  });

  it('converts headings and paragraphs to HTML', () => {
    const html = service.render('# Summary\n\nA concise overview.');

    expect(html).toContain('<h1>Summary</h1>');
    expect(html).toContain('<p>A concise overview.</p>');
  });

  it('renders basic emphasis and lists', () => {
    const html = service.render('**Important**\n\n- First\n- Second');

    expect(html).toContain('<strong>Important</strong>');
    expect(html).toContain('<li>First</li>');
    expect(html).toContain('<li>Second</li>');
  });

  it('removes executable script elements from untrusted Markdown', () => {
    const html = service.render('<script>alert(1)</script>\n\nSafe text');

    expect(html).not.toContain('<script');
    expect(html).toContain('Safe text');
  });

  it('removes JavaScript URLs from generated links', () => {
    const html = service.render('[unsafe](javascript:alert(1))');

    expect(html).not.toContain('javascript:');
  });

  it('returns an empty string for empty Markdown', () => {
    expect(service.render('')).toBe('');
  });
});
