import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GameCard } from './game-card';
import { provideRouter } from '@angular/router';
import { GameModel } from '../../core/models/game.model';

const mockGame: GameModel = {
  id: 1,
  name: 'Test Game',
  backgroundImage: null,
  released: '2020-01-01',
  rating: 4,
  metacritic: 80,
  genres: [],
  platforms: [],
};

describe('GameCard', () => {
  let component: GameCard;
  let fixture: ComponentFixture<GameCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GameCard],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(GameCard);
    fixture.componentRef.setInput('game', mockGame);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
