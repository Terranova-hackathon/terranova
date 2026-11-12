import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GameDetails } from './game-details';

describe('GameDetails', () => {
  let component: GameDetails;
  let fixture: ComponentFixture<GameDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameDetails],
    }).compileComponents();

    fixture = TestBed.createComponent(GameDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
