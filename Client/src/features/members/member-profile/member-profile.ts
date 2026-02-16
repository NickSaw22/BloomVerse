import { Component, HostListener, inject, OnDestroy, OnInit, signal, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EditableMember, Member } from '../../../types/member';
import { DatePipe } from '@angular/common';
import { MemberService } from '../../../core/services/member-service';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastService } from '../../../core/services/toast-service';
import { AccountService } from '../../../core/services/account-service';
import { TimeAgoPipe } from '../../../core/pipes/time-ago-pipe';

@Component({
  selector: 'app-member-profile',
  imports: [DatePipe, FormsModule, TimeAgoPipe],
  templateUrl: './member-profile.html',
  styleUrl: './member-profile.css',
})
export class MemberProfile implements OnInit, OnDestroy {
  @ViewChild('editForm') editForm?: NgForm;
  @HostListener('window:beforeunload', ['$event']) notify($event: BeforeUnloadEvent) {
    if(this.editForm?.dirty) {
      $event.preventDefault();
    }
  }
  protected title = signal<string | undefined>('Profile');
  protected memberService = inject(MemberService);
  protected editableMember: EditableMember ={
    displayName: '',
    description: '',
    city: '',
    country: ''
  };
  private toastService = inject(ToastService);
  private accountService = inject(AccountService);

  ngOnInit() {
      this.editableMember = {
      displayName: this.memberService.member()?.displayName || '',
      description: this.memberService.member()?.description || '',
      city: this.memberService.member()?.city || '',
      country: this.memberService.member()?.country || ''
    };
  }

  updateProfile() {
    if(!this.memberService.member()) return; 
    const updatedMember = {...this.memberService.member(), ...this.editableMember };
    console.log('Updated Member Data:', updatedMember);
    this.memberService.updateMember(this.editableMember).subscribe({
      next: () => {
        let currentUser = this.accountService.currentUser();
        if(currentUser && currentUser.displayName !== updatedMember.displayName) {
          this.accountService.setCurrentUser({...currentUser, displayName: updatedMember.displayName});
        }
        this.toastService.success('Profile updated successfully!', 3000);
        this.memberService.editMode.set(false);
        this.memberService.member.set(updatedMember as Member);
        this.editForm?.reset(updatedMember);
      },
      error: err => {
        console.error('Error updating profile:', err);
        this.toastService.error(err.error || 'Failed to update profile. Please try again.', 5000);
      }
    });
    this.toastService.success('Profile updated successfully!', 3000);
    this.memberService.editMode.set(false);
  }

  ngOnDestroy(): void {
    if(this.memberService.editMode()) {
      this.memberService.editMode.set(false);
    }
  }
}
