import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../../types/member';
import { Router, RouterLink } from '@angular/router';
import { AgePipe } from '../../../core/pipes/age-pipe';
import { LikesService } from '../../../core/services/likes-service';
import { ToastService } from '../../../core/services/toast-service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink, AgePipe],
  templateUrl: './member-card.html',
  styleUrl: './member-card.css',
})
export class MemberCard {
  private likesService = inject(LikesService);
  member = input.required<Member>();
  hasLiked = computed(() => this.likesService.likeIds().includes(this.member().id));
  private toastr = inject(ToastService);
  protected currentUser = (() => {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  })();
  toggleLike(event: Event){
    event.stopPropagation();
    this.likesService.toggleLike(this.member().id).subscribe({
      next: () =>{ 
        if(this.hasLiked()) {
          this.likesService.likeIds.set(this.likesService.likeIds().filter(id => id !== this.member().id));
          this.toastr.info('You have unliked ' + this.member().displayName);
        } else {
          this.likesService.likeIds.set([...this.likesService.likeIds(), this.member().id]);
          this.toastr.success('You have liked ' + this.member().displayName);
        }
      }
    })
  }
}
