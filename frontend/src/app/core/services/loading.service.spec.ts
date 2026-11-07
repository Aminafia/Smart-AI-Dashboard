import { TestBed } from '@angular/core/testing';

import { LoadingService } from './loading.service';

describe('LoadingService', () => {
  let service: LoadingService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoadingService);
  });

  it('starts in the not-loading state', () => {
    let currentState: boolean | undefined;
    service.loading$.subscribe(value => currentState = value);

    expect(currentState).toBe(false);
  });

  it('emits true when a request starts and false when it finishes', () => {
    const states: boolean[] = [];
    service.loading$.subscribe(value => states.push(value));

    service.show();
    service.hide();

    expect(states).toEqual([false, true, false]);
  });

  it('keeps the overlay active until all concurrent requests finish', () => {
    const states: boolean[] = [];
    service.loading$.subscribe(value => states.push(value));

    service.show();
    service.show();
    service.hide();

    expect(states.at(-1)).toBe(true);

    service.hide();

    expect(states.at(-1)).toBe(false);
  });

  it('does not become stuck when hide is called without a matching show', () => {
    const states: boolean[] = [];
    service.loading$.subscribe(value => states.push(value));

    service.hide();

    expect(states.at(-1)).toBe(false);
  });
});
