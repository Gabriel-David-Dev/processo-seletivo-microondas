import { TestBed } from '@angular/core/testing';

import { MicroondasService } from './microondas.service';

describe('MicroondasService', () => {
  let service: MicroondasService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(MicroondasService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
