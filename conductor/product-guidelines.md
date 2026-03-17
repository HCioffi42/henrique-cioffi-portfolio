# Product Guidelines

## Development Principles
- **Clean Architecture:** Maintain a clear separation of concerns between layers (Domain, Application, Infrastructure, API).
- **Test-Driven Development (TDD):** Prefer writing tests before or alongside implementation to ensure code reliability.
- **SOLID Principles:** Follow object-oriented design principles to ensure maintainability and scalability.
- **Clear Documentation:** Maintain up-to-date XML comments for C# and JSDoc for TypeScript, alongside high-quality Markdown documentation.
- **Language Standard:** All code (classes, variables, methods), commits, and technical documentation must be written in **English** to maintain professional standards and portability.
- **Mandatory Documentation:** Every new method, interface, or component must include descriptive documentation:
    - **C#**: Use XML Summary tags (`/// <summary>`) explaining the purpose, parameters, and return values.
    - **TypeScript/React**: Use JSDoc comments (`/** ... */`) for functions, interfaces, and hooks to ensure IntelliSense clarity in the JetBrains Rider.

## User Experience
- **Responsiveness:** All UI components must be mobile-friendly and adaptable to different screen sizes.
- **Performance:** Ensure API endpoints are optimized (using NoTracking and Pagination) and frontend components avoid unnecessary re-renders.

## Code Style
- **Naming (C#):** Follow standard .NET naming conventions (PascalCase for classes/methods/interfaces, camelCase for local variables and private fields).
- **Naming (TypeScript):** Follow standard TS conventions (PascalCase for Components, Interfaces, and Types; camelCase for variables, functions, and hooks).
- **Asynchronous Code:** Prefer `async`/`await` for all I/O-bound operations in both backend and frontend.
- **Type Safety:** Use strong typing features effectively. Avoid the `any` type in TypeScript and prefer records or DTOs in C#.
- **Component Pattern:** Prefer Functional Components with Hooks for React development.
- **File Naming:** Use PascalCase for React components (e.g., `ArticleCard.tsx`) and camelCase for hooks, utility functions, or services (e.g., `useAuth.ts`, `apiService.ts`).
- **Strict Typing:** Always define interfaces or types for API responses and component props to maintain a predictable data flow.

### Workflow and Documentation Standards
- ALWAYS create a subfolder within the `Tracks` directory for every new feature or significant task.
- Each track folder MUST contain a `plan.md` (detailing the intended steps before coding) and an `index.md` (summarizing the final implementation).
- Automatically update the root `Track` registry file to reflect the progress and status of the current task.
- Ensure all technical documentation follows a concise, professional tone, describing logic in the third person.
- Always show a detailed plan before starting implementation, and provide a comprehensive summary after completion to ensure clarity and maintainability.