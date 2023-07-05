<template>
  <div class="text-center">
    <q-avatar class="q-mb-md" style="width: 200px; height: 100px" square>
      <q-img :src="`/img/logo.png`" />
    </q-avatar>

    <div class="text-h2 q-mt-lg">Brands</div>

    <q-btn
      class="q-ma-sm"
      color="white"
      text-color="black"
      label="Load Brands"
      @click="loadBrands"
    />

    <div class="status q-mt-md text-subtitle2 text-negative" text-color="red">
      {{ state.status }}
    </div>

    <p></p>
    <li v-for="brand in state.brands" v-bind:key="brand.id">
      {{ brand.name }}
    </li>
  </div>
</template>

<script>
import { reactive } from "vue";
import { fetcher } from "../utils/apiutil";

export default {
  setup() {
    let state = reactive({
      status: "",
      brands: [],
    });

    const loadBrands = async () => {
      try {
        state.brands = await fetcher(`Brand`);
        state.status = `found  ${state.brands.length} brands`;
      } catch (err) {
        console.log(err);
        state.status = `Error has occured: ${err.message}`;
      }
    };

    return {
      loadBrands,
      state,
    };
  },
};
</script>
