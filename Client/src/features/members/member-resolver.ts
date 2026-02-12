import { ResolveFn, Router } from '@angular/router';
import { EMPTY, map } from 'rxjs';
import { MemberService } from '../../core/services/member-service';
import { inject } from '@angular/core';
import { Member } from '../../types/member';

export const memberResolver: ResolveFn<Member> = (route, state) => {
  const memeberService = inject(MemberService);
  const memberId = route.paramMap.get('id');
  const router = inject(Router);
  
  if (!memberId) {
    router.navigateByUrl('/not-found');
    return EMPTY;
  }
  
  return memeberService.getMember(memberId);
};
