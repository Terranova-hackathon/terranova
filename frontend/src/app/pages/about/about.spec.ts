import { ComponentFixture, TestBed } from '@angular/core/testing';
import { About } from './about';

describe('About', () => {
  let component: About;
  let fixture: ComponentFixture<About>;
  let el: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [About],
    }).compileComponents();

    fixture = TestBed.createComponent(About);
    component = fixture.componentInstance;
    await fixture.whenStable();
    fixture.detectChanges();
    el = fixture.nativeElement;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('renders one card per author', () => {
    expect(el.querySelectorAll('.about__author').length).toBe(component.authors.length);
  });

  it('links each author to GitHub and LinkedIn in a new tab', () => {
    const first = component.authors[0];
    const links = el.querySelectorAll<HTMLAnchorElement>('.about__author:first-child a');

    expect(links[0].getAttribute('href')).toBe(first.githubUrl);
    expect(links[1].getAttribute('href')).toBe(first.linkedinUrl);
    links.forEach((link) => {
      expect(link.target).toBe('_blank');
      expect(link.rel).toContain('noopener');
    });
  });
});
