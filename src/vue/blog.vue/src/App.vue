<template>
  <div id="app">
    <div id="nav">
      <router-link to="/">Home</router-link> |
      <router-link to="/form">Form</router-link> |
      <router-link to="/Vuex">FormVuex</router-link> |
      <router-link to="/Login">Login</router-link>
    </div>
    <router-view />
  </div>
</template>
<style lang="css">
@import "./style/stylehome.css";
@import "./style/base.css";
@import "./style/m.css";
</style>
<style lang="stylus">
#app
  font-family Avenir, Helvetica, Arial, sans-serif
  -webkit-font-smoothing antialiased
  -moz-osx-font-smoothing grayscale
  text-align center
  color #2c3e50
  margin-top 60px
</style>
