import { TestBed } from '@angular/core/testing';
import { BackendConfigService } from './backend-config';

describe('BackendConfig', () => {
  let service: BackendConfigService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BackendConfigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
