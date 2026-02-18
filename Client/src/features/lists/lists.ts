import { Component, inject, OnInit, signal } from '@angular/core';
import { Member } from '../../types/member';
import { MemberService } from '../../core/services/member-service';
import { PaginatedResult, Pagination } from '../../types/pagination';
import { LikesService } from '../../core/services/likes-service';
import { MemberCard } from '../members/member-card/member-card';
import { FormsModule } from '@angular/forms';
import { Paginator } from '../../shared/paginator/paginator';
@Component({
  selector: 'app-lists',
  imports: [FormsModule, MemberCard, Paginator],
  templateUrl: './lists.html',
  styleUrl: './lists.css',
})
export class Lists implements OnInit {
  private likesService = inject(LikesService);
  members = signal<Member[]>([]);
  predicate = 'liked';
  pageNumber = 1;
  pageSize = 5;
  protected paginatedResult = signal<PaginatedResult<Member> | null>(null);
  // pagination: Pagination | undefined;
  tabs = [
    { label: 'Liked', value: 'liked' },
    { label: 'Liked By', value: 'likedBy' },
    { label: 'Mutual Likes', value: 'mutual' },
  ]
  ngOnInit(): void {
    this.loadLikes();
  }

  loadLikes() {
    this.likesService.getLikes(this.predicate, this.pageNumber, this.pageSize).subscribe({
      next: res => this.paginatedResult.set(res),
      error: error => console.log(error)
    });
  }
  setPredicate(predicate: string) {
    this.predicate = predicate;
    this.pageNumber = 1;
    this.loadLikes();
  }
  pageChanged(event: {pageNumber: number, pageSize: number}) {
    this.pageNumber = event.pageNumber;
    this.pageSize = event.pageSize;
    this.loadLikes();
  }
}
