import { HttpEvent, HttpInterceptorFn } from '@angular/common/http';
import { delay, finalize, of, tap } from 'rxjs';
import { BusyService } from '../services/busy-service';
import { inject } from '@angular/core';

const cache = new Map<string, HttpEvent<any>>();

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);

  if(req.method === 'GET') {
    const cachedResponse = cache.get(req.url);
    if(cachedResponse) {
      return of(cachedResponse);
    }
  }

  busyService.busy();
  return next(req).pipe(
    delay(500),
    tap(response=> {
      if(req.method === 'GET') {
        cache.set(req.url, response);
      }
    }),
    finalize(() => busyService.idle())
  );
};
