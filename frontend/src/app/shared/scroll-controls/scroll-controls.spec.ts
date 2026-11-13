import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScrollControls } from './scroll-controls';

describe('ScrollControls', () => {
  let component: ScrollControls;
  let fixture: ComponentFixture<ScrollControls>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScrollControls],
    }).compileComponents();

    fixture = TestBed.createComponent(ScrollControls);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
