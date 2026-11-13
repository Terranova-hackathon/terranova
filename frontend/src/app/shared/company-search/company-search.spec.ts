import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CompanySearch } from './company-search';

describe('CompanySearch', () => {
  let component: CompanySearch;
  let fixture: ComponentFixture<CompanySearch>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompanySearch],
    }).compileComponents();

    fixture = TestBed.createComponent(CompanySearch);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
