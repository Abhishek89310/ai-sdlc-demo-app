import { defineConfig, devices } from '@playwright/test';

// macOS reserves port 5000 for AirPlay; set E2E_PORT to run the app elsewhere.
const port = process.env.E2E_PORT ?? '5000';
const baseURL = `http://localhost:${port}`;

export default defineConfig({
  testDir: './tests',
  workers: 1,
  retries: process.env.CI ? 1 : 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL,
    trace: 'on-first-retry',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: 'dotnet run --no-launch-profile --project ../backend/src/InvoiceApp.Api',
    env: { ASPNETCORE_URLS: baseURL },
    url: baseURL,
    reuseExistingServer: true,
    timeout: 120_000,
  },
});
