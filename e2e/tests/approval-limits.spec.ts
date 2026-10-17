import { expect, test } from '@playwright/test';
import { ApprovalsPage } from '../pages/ApprovalsPage';

test('a Manager approving an over-limit invoice is refused with a reason and the invoice stays pending', async ({ page }) => {
  const approvals = new ApprovalsPage(page);
  await approvals.signIn('demo.manager', 'ChangeMe123!');
  await approvals.openApprovals();

  await expect(approvals.pendingInvoice('INV-1006')).toContainText('$75,000.00');
  await approvals.approve('INV-1006');

  const refusal = approvals.refusalAlert('A Manager can approve up to 50,000');
  await expect(refusal).toBeVisible();
  await expect(refusal).toContainText('A Director must approve it');
  await expect(approvals.pendingInvoice('INV-1006')).toBeVisible();
  await expect(approvals.pendingInvoice('INV-1006')).not.toContainText('First approval');
});
