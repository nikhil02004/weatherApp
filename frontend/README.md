# Weather App Frontend

This is an Angular 21 application that connects to the Weather API backend.

## Prerequisites

- Node.js (v18 or higher)
- npm (v9 or higher)

## Installation

```bash
npm install
```

## Running the Application

1. Make sure your backend API is running (typically on http://localhost:5000)
2. Start the Angular development server:

```bash
npm start
```

3. Navigate to `http://localhost:4200/` in your browser

## Features

- User Authentication (Login/Register)
- Weather Search by City
- Current Weather Display
- 5-Day Weather Forecast
- Responsive Design

## Project Structure

- `src/app/components/` - Angular components
- `src/app/services/` - API services
- `src/app/models/` - TypeScript interfaces
- `src/app/guards/` - Route guards
- `src/app/interceptors/` - HTTP interceptors

## Configuration

Update the API URL in the service files if your backend is running on a different port:
- `src/app/services/auth.service.ts`
- `src/app/services/weather.service.ts`
