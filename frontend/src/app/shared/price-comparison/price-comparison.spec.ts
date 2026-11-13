import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PriceComparison } from './price-comparison';

describe('PriceComparison', () => {
  let component: PriceComparison;
  let fixture: ComponentFixture<PriceComparison>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PriceComparison],
    }).compileComponents();

    fixture = TestBed.createComponent(PriceComparison);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
