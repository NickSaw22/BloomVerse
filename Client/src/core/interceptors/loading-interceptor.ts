import { HttpEvent, HttpInterceptorFn, HttpParams } from '@angular/common/http';
import { delay, finalize, of, tap } from 'rxjs';
import { BusyService } from '../services/busy-service';
import { inject } from '@angular/core';

const cache = new Map<string, HttpEvent<any>>();

export const loadingInterceptor: HttpInterceptorFn = (req, next) => {
  const busyService = inject(BusyService);

  const generateCacheKey = (url: string, params: HttpParams): string => {
    const paramString = params.keys().map(key => `${key}=${params.get(key)}`).join('&');
    return paramString ? `${url}?${paramString}` : url;
  }

  const cacheKey = generateCacheKey(req.url, req.params);

  const invalidateCache = (urlPattern: string) => {
    for(const key of cache.keys()) {
      if(key.includes(urlPattern)) {
        cache.delete(key);
        console.log(`Cache invalidated for pattern: ${key}`);
      }
    }
  }
  
  if(req.method.includes('POST') && req.url.includes('/likes')) {
    invalidateCache('/likes');
  }

  if(req.method === 'GET') {
    const cachedResponse = cache.get(cacheKey);
    if(cachedResponse) {
      return of(cachedResponse);
    }
  }

  busyService.busy();
  return next(req).pipe(
    delay(500),
    tap(response=> {
      if(req.method === 'GET') {
        cache.set(cacheKey, response);
      }
    }),
    finalize(() => busyService.idle())
  );
};
