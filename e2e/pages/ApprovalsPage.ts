import { expect, type Page } from '@playwright/test';

export class ApprovalsPage {
  constructor(private readonly page: Page) {}

  async signIn(username: string, password: string) {
    await this.page.goto('/');
    await this.page.getByLabel('Username').fill(username);
    await this.page.getByLabel('Password').fill(password);
    await this.page.getByRole('button', { name: 'Sign in' }).click();
    await expect(this.page.getByRole('tab', { name: 'Approvals' })).toBeVisible();
  }

  async openApprovals() {
    await this.page.getByRole('tab', { name: 'Approvals' }).click();
  }

  async approve(invoiceId: string) {
    await this.page.getByRole('button', { name: `Approve invoice ${invoiceId}` }).click();
  }

  refusalAlert() {
    return this.page.getByRole('alert').filter({ hasText: /Manager|Director|approver/ });
  }

  pendingInvoice(invoiceId: string) {
    return this.page.getByRole('listitem').filter({ hasText: invoiceId });
  }
}
