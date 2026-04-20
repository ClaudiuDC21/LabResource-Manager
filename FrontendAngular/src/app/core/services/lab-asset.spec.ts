import { TestBed } from '@angular/core/testing';

import { LabAsset } from './lab-asset';

describe('LabAsset', () => {
  let service: LabAsset;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LabAsset);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
