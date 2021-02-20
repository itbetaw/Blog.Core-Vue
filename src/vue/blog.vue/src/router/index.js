import Vue from "vue";
import VueRouter from "vue-router";
import Home from "../views/Home.vue";
import FormVuex from "../views/FormVuex.vue";
import Login from "../views/Login.vue";
import Content from "../views/content.vue";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "Home",
    component: Home,
    meta: { requireAuth: true },
  },

  {
    path: "/form",
    name: "Form",
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () =>
      import(/* webpackChunkName: "about" */ "../views/Form.vue"),
  },
  {
    path: "/Vuex",
    name: "Vuex",
    component: FormVuex,
  },
  {
    path: "/Content/:id",
    name: "Content",
    component: Content,
    meta: {
      requireAuth: true // 添加该字段，表示进入这个路由是需要登录的
    }
  },
  {
    path: "/Login",
    name: "Login",
    component: Login,
  },
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes,
});

router.beforeEach((to, from, next) => {
  if (to.meta.requireAuth) {
    if (window.localStorage.Token && window.localStorage.Token.length >= 128) {
      next();
    } else {
      next({
        path: "/login",
        query: { redirect: to.fullPath }, // 将跳转的路由path作为参数，登录成功后跳转到该路由
      });
    }
  } else {
    next();
  }
});

export default router;
