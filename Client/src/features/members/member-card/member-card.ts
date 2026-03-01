import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { Router, RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';
import { ToastService } from '../../../core/services/toast-service';
import { PresenceService } from '../../../core/services/presence-service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css',
})
export class MemberCard {
  private likesService = inject(LikesService);
  private presenceService = inject(PresenceService);
  member = input.required<Member>();
  hasLiked = computed(() => this.likesService.likeIds().includes(this.member().id));
  private toastr = inject(ToastService);
  isOnline = computed(() => this.presenceService.onlineUsers().includes(this.member().id));

  protected currentUser = (() => {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  })();
  toggleLike(event: Event){
    event.stopPropagation();
    this.likesService.toggleLike(this.member().id);
    // if(this.hasLiked()) {
    //   this.toastr.success(`You liked ${this.member().displayName}'s profile!`, 3000);
    // } else {
    //   this.toastr.info(`You unliked ${this.member().displayName}'s profile.`, 3000);
    // }
  }
}
