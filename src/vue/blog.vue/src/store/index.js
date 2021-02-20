import Vue from "vue";
import Vuex, { Store } from "vuex";

Vue.use(Vuex);

export default new Vuex.Store({
  state: {
    formDatas: null, // 定义一个变量formDatas
    token: "1",
  },
  mutations: {
    getFormData(state, data) {
      state.formDatas = data;
    },
    saveToken(state, data) {
      state.token = data;
      window.localStorage.setItem("Token", data);
    },
  },
  actions: {},
  modules: {},
});
