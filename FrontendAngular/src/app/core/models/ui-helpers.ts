import { UserRole, BorrowingStatus, AssetStatus } from './enums';
import { UserResponse } from './user';

export class UIHelpers {
  static getRoleName(role: UserRole | string | number | undefined): string {
    if (role === undefined || role === null) return 'Unknown';
    const roleValue = typeof role === 'string' ? Number.parseInt(role, 10) : role;
    return roleValue === UserRole.Teacher ? 'Teacher' : 'Student';
  }

  static getRoleSeverity(role: UserRole | string | number | undefined): 'success' | 'info' {
    if (role === undefined || role === null) return 'info';
    const roleValue = typeof role === 'string' ? Number.parseInt(role, 10) : role;
    return roleValue === UserRole.Teacher ? 'info' : 'success';
  }

  static getBorrowingStatusSeverity(status: BorrowingStatus): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case BorrowingStatus.Active: return 'success';
      case BorrowingStatus.Pending: return 'info';
      case BorrowingStatus.Approved: return 'warn';
      case BorrowingStatus.Returned: return 'secondary';
      case BorrowingStatus.Rejected: return 'danger';
      default: return 'info';
    }
  }

  static getBorrowingStatusName(status: BorrowingStatus): string {
    const names = {
      [BorrowingStatus.Pending]: 'Pending',
      [BorrowingStatus.Approved]: 'Approved',
      [BorrowingStatus.Active]: 'Active',
      [BorrowingStatus.Returned]: 'Returned',
      [BorrowingStatus.Rejected]: 'Rejected'
    };
    return names[status] || 'Unknown';
  }

  static getAssetStatusSeverity(status: AssetStatus): 'success' | 'info' | 'warn' | 'danger' {
    switch (status) {
      case AssetStatus.Available: return 'success';
      case AssetStatus.PendingApproval: return 'info';
      case AssetStatus.Borrowed: return 'warn';
      case AssetStatus.Defective: return 'danger';
      default: return 'success';
    }
  }

  static getAssetStatusName(status: AssetStatus): string {
    const names = {
      [AssetStatus.Available]: 'Available',
      [AssetStatus.PendingApproval]: 'Pending Approval',
      [AssetStatus.Borrowed]: 'Borrowed',
      [AssetStatus.Defective]: 'Defective'
    };
    return names[status] || 'Unknown';
  }

  static getTimelinessStatus(endDate: string, actualReturnDate?: string | null): string {
    const expected = new Date(endDate).getTime();
    const actual = actualReturnDate ? new Date(actualReturnDate).getTime() : Date.now();
    return actual > expected ? 'Exceeded' : 'On Time';
  }

  static getTimelinessSeverity(endDate: string, actualReturnDate?: string | null): 'success' | 'danger' {
    const expected = new Date(endDate).getTime();
    const actual = actualReturnDate ? new Date(actualReturnDate).getTime() : Date.now();
    return actual > expected ? 'danger' : 'success';
  }

  static isUserActive(userObj: UserResponse | null): boolean {
    if (!userObj) return false;
    return userObj.isActive === true || String(userObj.isActive).toLowerCase() === 'true';
  }

  static calculateProgress(startDate: string, endDate: string, status: BorrowingStatus): { progressValue: number, timeLeftLabel: string, isExceeded: boolean } {
    const start = new Date(startDate).getTime();
    const end = new Date(endDate).getTime();
    const now = Date.now();

    if (status === BorrowingStatus.Pending || status === BorrowingStatus.Approved) {
      return {
        progressValue: 0,
        timeLeftLabel: status === BorrowingStatus.Pending ? 'Awaiting Approval' : 'Waiting for pick-up',
        isExceeded: false
      };
    }

    const totalDuration = end - start;
    const elapsed = now - start;

    let progress = 0;
    if (totalDuration > 0) {
        progress = Math.round((elapsed / totalDuration) * 100);
    }
    
    progress = Math.max(0, Math.min(100, progress));
    
    const isExceeded = now > end;
    let timeLeftLabel = '';

    if (isExceeded) {
      timeLeftLabel = 'Overdue';
    } else if (now < start) {
      timeLeftLabel = 'Not started yet';
    } else {
      const msLeft = end - now;
      const hoursLeft = Math.floor(msLeft / (1000 * 60 * 60));
      const daysLeft = Math.floor(hoursLeft / 24);

      if (daysLeft > 0) {
        timeLeftLabel = `${daysLeft}d ${hoursLeft % 24}h left`;
      } else {
        timeLeftLabel = `${hoursLeft}h left`;
      }
    }

    return {
      progressValue: progress,
      timeLeftLabel: timeLeftLabel,
      isExceeded: isExceeded
    };
  }
}