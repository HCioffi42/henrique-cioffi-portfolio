# Track: Implement Unit Tests for Artigo Validators

**Track ID**: `unit_tests_validators_20260314`
**Goal**: Implement unit tests for `CreateArtigoCommandValidator` and `UpdateArtigoCommandValidator` using xUnit and `FluentValidation.TestHelper`.

## Implementation Plan

### 1. Setup Phase
- Install `FluentValidation.TestHelper` in `MeuSitePessoal.Tests`.
- Create the target folder: `MeuSitePessoal.Tests/Unit/Application/Artigos/Validators`.

### 2. Implementation: CreateArtigoCommandValidatorTests
- Test scenarios for:
  - **Title**: Error when empty, null, whitespace, or exceeding 100 characters.
  - **Content**: Error when empty or null.
  - **Summary**: Error when empty, null, whitespace, or exceeding 500 characters.
- Success scenario for valid data.
- Boundary test for Title (length 101).
- Boundary test for Summary (length 501).

### 3. Implementation: UpdateArtigoCommandValidatorTests
- Test scenarios for:
  - **Id**: Error when empty Guid.
  - **Title**: Error when empty, null, whitespace, or exceeding 100 characters.
  - **Content**: Error when empty or null.
  - **Summary**: Error when empty, null, whitespace, or exceeding 500 characters.
- Success scenario for valid data.
- Boundary test for Title (length 101).
- Boundary test for Summary (length 501).

### 4. Mandatory Refinements
- Use `[Theory]` and `[InlineData]` for edge cases (null, empty, whitespace).
- Naming convention: `Should_Have_Error_When_[Property]_[Reason]`.
- All code comments in English.

## Verification Strategy
- Run `dotnet test --filter Validators` and ensure all tests pass.
