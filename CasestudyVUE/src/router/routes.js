const routes = [
  {
    path: "/",
    component: () => import("layouts/MainLayout.vue"),
    children: [
      // Home Page
      {
        path: "/",
        name: "home",
        component: () => import("pages/HomePage.vue"),
      },
      // Brand List Page
      {
        path: "/brands",
        name: "brands",
        component: () => import("pages/BrandListPage.vue"),
      },
      // Cart Page
      {
        path: "/cart",
        name: "cart",
        component: () => import("pages/CartPage.vue"),
      },
      // Register Page
      {
        path: "/register",
        name: "register",
        component: () => import("pages/RegisterPage.vue"),
      },
      // Login Page
      {
        path: "/login",
        name: "login",
        component: () => import("pages/LoginPage.vue"),
      },
      // Logout Page
      {
        path: "/logout",
        name: "logout",
        component: () => import("pages/LogoutPage.vue"),
      },
    ],
  },
  // Always leave this as last one,
  // but you can also remove it
  {
    path: "/:catchAll(.*)*",
    component: () => import("pages/ErrorNotFound.vue"),
  },
];
export default routes;
