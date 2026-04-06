- [x] **Track 16.1: Anonymous Newsletter System**
  *Link: [./tracks/16-community/index.md](./tracks/16-community/index.md)*
- [x] **Track 16.2: Nested Comment System**
  *Link: [./tracks/16-community/index.md](./tracks/16-community/index.md)*

## Final Status
**Completed**

## Summary
Successfully implemented the Anonymous Newsletter System and the Nested Comment System.

### 16.1 Newsletter
- Created `Result<T>` pattern for robust handler responses.
- Implemented `Subscriber` domain entity and updated the PostgreSQL database context (`BlogDbContext`) to include a unique index.
- Applied EF Core migrations.
- Set up MediatR command and handler for `SubscribeToNewsletter`, including FluentValidation.
- Created an API `[AllowAnonymous]` endpoint inside `NewsletterController`.
- Developed `NewsletterBox.tsx` using Tailwind CSS v4.

### 16.2 Nested Comment System
- Implemented `Comment` entity with self-referencing relationship for threading.
- Configured recursive deletion and constraints in `BlogDbContext`.
- Developed CQRS handlers for comment creation and retrieval.
- Implemented an **In-Memory Tree Assembly Strategy** in the backend to return a full hierarchy in a single $O(n)$ pass.
- Created recursive `CommentItem.tsx` components for unlimited nesting depth.
- Integrated the discussion section into the Article Details view.
