import { Component } from '@angular/core';

export interface Author {
  name: string;
  role: string;
  blurb: string;
  githubUrl: string;
  linkedinUrl: string;
}

@Component({
  selector: 'app-about',
  styleUrl: './about.scss',
  templateUrl: './about.html',
})
export class About {
  readonly authors: readonly Author[] = [
    {
      name: 'Kelvin Lemayian',
      role: 'Lead Frontend Developer',
      blurb: 'Built the Angular frontend, from game search and filters to favorites and themes.',
      githubUrl: 'https://github.com/Blackelv/',
      linkedinUrl: 'https://linkedin.com/in/kevin-lemayian-15b3171b5',
    },
    {
      name: 'Minoka Kakizaki',
      role: 'Lead Backend Developer',
      blurb: 'Designed the ASP.NET Core API, the external API integrations, and authentication system.',
      githubUrl: 'https://github.com/kakizaki55',
      linkedinUrl: 'https://linkedin.com/in/minoka-kakizaki',
    },
    {
      name: 'Francesca Frattaroli',
      role: 'Testing, Documentation & Deployment',
      blurb: 'Keeps LunaNova tested, documented and running.',
      githubUrl: 'https://github.com/francescafr',
      linkedinUrl: 'https://linkedin.com/in/ffrattaroli',
    },
  ];
}
