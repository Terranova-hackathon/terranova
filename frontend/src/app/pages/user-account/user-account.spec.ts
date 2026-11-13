import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserAccount } from './user-account';

describe('UserAccount', () => {
  let component: UserAccount;
  let fixture: ComponentFixture<UserAccount>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserAccount],
    }).compileComponents();

    fixture = TestBed.createComponent(UserAccount);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
