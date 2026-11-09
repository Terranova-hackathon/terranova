import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GameFilters } from './game-filters';

describe('GameFilters', () => {
  let component: GameFilters;
  let fixture: ComponentFixture<GameFilters>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameFilters],
    }).compileComponents();

    fixture = TestBed.createComponent(GameFilters);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
