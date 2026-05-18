# MockServer

This folder contains the local MockServer expectations used by the application layer.

## Expectations

- `expectations/payment-success.json` - success response for `POST /psp/pay`
- `expectations/payment-error.json` - error response for `POST /psp/pay/error`
- `expectations/payment-timeout.json` - delayed response for `POST /psp/pay/timeout`

## Seeding

Use the PowerShell script in `scripts/seed-mockserver.ps1` to load the expectations into a running MockServer instance.

## Quick test flow

1. Start MockServer: `docker compose up -d mock-server`
2. Seed the expectations: `powershell -ExecutionPolicy Bypass -File .\scripts\seed-mockserver.ps1`
3. Test the app endpoint: `POST /payments/mock`

## Direct MockServer endpoints

- `POST /psp/pay` returns a success response
- `POST /psp/pay/error` returns an error response
- `POST /psp/pay/timeout` returns a delayed response
