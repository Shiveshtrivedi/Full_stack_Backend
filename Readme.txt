Overview
• Project: E-commerce Platform
• Objective: Design a responsive, real-time application with a focus on modern 
user experience, scalability, and maintainability.
• Technologies Used:
• Frontend: React.js, Styled Components, Redux Toolkit, Axios, MQTT for realtime data.
• Backend: .NET Core WebAPI, Razorpay for payment processing, Mosquitto
Eclipse for MQTT, JWT for authentication, Bcrypt for password hashing, MSSQL 
for the database, and AutoMapper for object mapping.
• Tooling: GitHub Actions, Husky, Prettier, ESLint, and Code Rabbit to enforce 
quality, consistency and maintainability across the codebase.
In Time Tec – Company Confidential
Frontend Stack: React & Styled Components
• Why React?
• Component-Based Architecture: Promotes modularity and reusability; helps manage 
and scale complex UIs.
• Virtual DOM: Ensures efficient updates to the UI, enhancing speed and responsiveness.
• Why Styled Components?
• Scoped Styling: Keeps styles specific to each component, preventing CSS conflicts.
• Dynamic Styling: Easily adjust styles based on props (e.g., product in stock vs. out of 
stock).
• Comparison:
• Angular: Heavier, more opinionated framework. React is more flexible for modular, UIdriven applications.
• CSS Modules: Limited dynamic styling, which is better handled with Styled Components.
In Time Tec – Company Confidential
Data Handling: Redux Toolkit & Axios
• Redux Toolkit:
• Centralized State Management: Tracks complex state across the app, including user 
info, cart data, and more.
• Reduced Boilerplate: Simplifies code, making it easier to implement and manage state.
• Axios:
• Enhanced Error Handling: Unlike Fetch, Axios automatically throws errors for non-200 
responses, simplifying code.
• Interceptors: Customizable request and response handling (e.g., authentication tokens, 
global headers).
• Comparison:
• Context API: Good for small state needs, but less scalable for complex, large-scale 
applications.
• Fetch: Lightweight but lacks Axios’s built-in error handling and customization, requiring 
more configuration.
In Time Tec – Company Confidential
Real-Time Updates: MQTT
• Why MQTT?
• Lightweight Protocol: Optimized for real-time, low-latency updates, essential for 
features like live inventory updates.
• Reliable Delivery: MQTT’s Quality of Service (QoS) levels ensure reliable message 
delivery, even over unstable connections.
• Comparison with WebSockets:
• Delivery Guarantees: MQTT offers delivery options (QoS levels) that ensure 
reliable synchronization, unlike WebSockets, which don’t have built-in delivery 
guarantees.
In Time Tec – Company Confidential
Backend Technologies
Why .NET Core WebAPI?
• Robust Framework: Provides a strong foundation for building scalable and highperformance web APIs.
• Cross-Platform Support: Can run on Windows, macOS, and Linux, offering 
flexibility in deployment.
Why Razorpay?
• Secure Payment Processing: Facilitates smooth and secure transactions, 
enhancing user trust.
• Comprehensive API: Easy integration with various payment methods and 
currencies.
In Time Tec – Company Confidential
Why Mosquitto Eclipse?
• MQTT Broker Management: Efficiently handles real-time messaging, supporting 
the e-commerce platform's needs for live updates.
• Lightweight and Scalable: Optimized for low-latency communication, essential 
for performance.
Why JWT (JSON Web Tokens)?
• Secure Authentication: Ensures user credentials are protected and session 
management is streamlined.
• Statelessness: Allows scalable and efficient authorization without server-side 
session storage.
Why Bcrypt?
• Secure Password Hashing: Protects user passwords with strong hashing 
algorithms, preventing unauthorized access.
• Adaptive Hashing: Can adjust to increase computational cost over time, 
enhancing security.
In Time Tec – Company Confidential
• Why MSSQL?
• Relational Database Management: Provides a structured approach to 
data storage and retrieval, ideal for complex queries.
• Integration with .NET: Seamlessly integrates with .NET applications, 
ensuring optimal performance.
• Why AutoMapper?
• Object-to-Object Mapping: Simplifies data transfer between models 
and DTOs, reducing boilerplate code.
• Increased Efficiency: Streamlines data handling, making code cleaner 
and more maintainable.
In Time Tec – Company Confidential
Project Enhancements: Tooling & Quality Assurance
• GitHub Actions:
• CI/CD Automation: Runs tests and linting checks on every pull request, ensuring 
quality standards.
• Husky:
• Pre-Commit Hooks: Automatically runs Prettier and ESLint checks before 
committing code, enforcing consistency.
• Prettier & ESLint:
• Consistency: Standardizes code formatting, making it easier to maintain and 
review.
• Error Prevention: ESLint catches syntax and logical errors early, maintaining high 
code quality.
In Time Tec – Company Confidential
Summary
• Scalable & Performant Design:
• React and Redux enable a modular, flexible frontend; MQTT ensures real-time 
updates.
• Axios streamlines API calls, managing requests more efficiently than Fetch.
• Maintainable Codebase: Code quality tools (GitHub Actions, Husky, ESLint) 
maintain consistency and prevent errors.
• Responsive UX: Styled Components and visual enhancements like React Medium 
Image Zoom improve user engagement across all devices.
• Authentication & Authorization: Outlining how user management and security 
are implemented.
• Razorpay: Integrates secure payment solutions, facilitating smooth transactions 
for users while ensuring compliance with financial regulations.
• MQTT: Utilized for real-time updates, enhancing communication between the 
frontend and backend, particularly in applications requiring live data streaming.
