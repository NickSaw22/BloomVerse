import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MemberService } from '../../../core/services/member-service';
import { filter, Observable } from 'rxjs';
import { Member } from '../../../types/member';

@Component({
  selector: 'app-member-detailed',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './member-detailed.html',
  styleUrl: './member-detailed.css',
})
export class MemberDetailed implements OnInit {
  private memberService = inject(MemberService);
  private route = inject(ActivatedRoute);
  protected member = signal<Member | undefined>(undefined);
  protected title = signal<string | undefined>('Profile')
  private router = inject(Router);
  ngOnInit() {
    this.route.data.subscribe({
      next: data => { this.member.set(data['member']); },
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
    const id = this.route.snapshot.paramMap.get('id');
    return this.memberService.getMember(id!);
  }
}