import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssetRequest } from './asset-request';

describe('AssetRequest', () => {
  let component: AssetRequest;
  let fixture: ComponentFixture<AssetRequest>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssetRequest],
    }).compileComponents();

    fixture = TestBed.createComponent(AssetRequest);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
