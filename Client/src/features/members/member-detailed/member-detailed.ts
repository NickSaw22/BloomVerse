import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MemberService } from '../../../core/services/member-service';
import { filter, Observable } from 'rxjs';
import { Member } from '../../../types/member';
import { AccountService } from '../../../core/services/account-service';
import { PresenceService } from '../../../core/services/presence-service';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';

@Component({
  selector: 'app-member-detailed',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, AgePipe],
  templateUrl: './member-detailed.html',
  styleUrl: './member-detailed.css',
})
export class MemberDetailed implements OnInit {
  protected memberService = inject(MemberService);
  private route = inject(ActivatedRoute);
  protected title = signal<string | undefined>('Profile')
  protected presenceService = inject(PresenceService);
  protected likesService = inject(LikesService);
  private router = inject(Router);
  private accountService = inject(AccountService);
  private routeId = signal<string | null>(null);
  protected isCurrentUser = computed(() => {
    return this.accountService.currentUser()?.id === this.routeId();
  });  
  protected hasLiked = computed(() => this.likesService.likeIds().includes(this.routeId()!));

  constructor() {
      this.route.paramMap.subscribe(params => {
        this.routeId.set(params.get('id'));
      });
  }
    
  ngOnInit() {
    this.route.data.subscribe({
      next: data => { this.memberService.member.set(data['member']); },
      error: err => { console.error('Error loading member data:', err); }
    });
    this.title.set(this.route.firstChild?.snapshot?.title)
    
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd))
      .subscribe({
        next: () => {
          this.title.set(this.route.firstChild?.snapshot?.title)
        }
      });
  }

  loadMember(): Observable<Member> {
    const id = this.routeId();
    return this.memberService.getMember(id!);
  }
}