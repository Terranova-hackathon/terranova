import { TestBed } from '@angular/core/testing';
import { GamePriceService } from './game-price-service';

describe('GamePriceService', () => {
  let service: GamePriceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GamePriceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
