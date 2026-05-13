# AutoSeller

Hi! 👋

This is my student project built to practice **C#** and modern **.NET frameworks** through both desktop and web development.

## Project Overview

This repository contains two parts:

- **ShopSellerUltra** — a desktop car shop app built with **WPF**.
- **Web_Registration** — a web app built with **ASP.NET Core (Razor Pages)** for user registration and authentication.

## Main Features

### Desktop app (WPF)

- Browse available cars in a catalog view.
- Filter cars by category (for example: Sedan, Truck, Hatchback, Electric, Sport).
- View product details such as image, category, price, and stock.
- Add items to cart and view total price.
- Clear the cart or complete a purchase flow.
- Open login/registration windows for user actions.
- Use promo codes that are stored in the database and have an expiration date

### Web app (ASP.NET Core)

- User registration form with email verification.
- User login endpoint/workflow.
- Basic API-style response models for auth operations.
- Service layer for database write/check logic.

## Technologies Used

- **C#**
- **.NET**
- **WPF (XAML)** for desktop UI
- **ASP.NET Core Razor Pages** for web UI
- **ASP.NET Core Web API/MVC components** (controllers, DTOs, services)
- **HTML/CSS/JavaScript** (web frontend)
- **Bootstrap + jQuery validation** (client-side web support)

## SQL Database Usage

The web part uses an SQL database as the persistent storage layer for user accounts.
In practice, the service layer sends SQL queries through the ASP.NET Core data access stack to:

- save new users during registration;
- check whether a user with a given email/login already exists;
- validate credentials during login.

## Interface Screenshot


![Main application window](interface.png)

## Note

This is a learning project, so some parts may be unfinished or simplified. Still, it has been a great hands-on step in learning C# and .NET development.
