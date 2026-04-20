import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyBorrowings } from './my-borrowings';

describe('MyBorrowings', () => {
  let component: MyBorrowings;
  let fixture: ComponentFixture<MyBorrowings>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyBorrowings],
    }).compileComponents();

    fixture = TestBed.createComponent(MyBorrowings);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
