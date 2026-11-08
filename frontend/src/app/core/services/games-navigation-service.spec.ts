import { TestBed } from '@angular/core/testing';
import { GamesNavigationService } from './games-navigation-service';

describe('GamesNavigationService', () => {
  let service: GamesNavigationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GamesNavigationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
