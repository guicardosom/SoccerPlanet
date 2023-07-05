<template>
  <div class="text-center">
    <q-avatar class="q-mb-md" style="width: 200px; height: 100px" square>
      <q-img :src="`/img/logo.png`" />
    </q-avatar>

    <div class="text-h2 q-mt-lg">Brands</div>

    <div class="status q-mt-md text-subtitle2 text-negative" text-color="red">
      {{ state.status }}
    </div>

    <q-select
      class="q-mt-lg q-ml-lg"
      v-if="state.brands.length > 0"
      style="width: 50vw; margin-bottom: 4vh; background-color: #fff"
      :option-value="'id'"
      :option-label="'name'"
      :options="state.brands"
      label="Select a Brand"
      v-model="state.selectedBrandId"
      @update:model-value="getProducts()"
      emit-value
      map-options
    />

    <div
      class="text-h6 text-bold text-center text-primary"
      v-if="state.products.length > 0"
    >
      {{ state.selectedBrand.name }} ITEMS
    </div>
    <q-scroll-area style="height: 55vh">
      <q-card class="q-ma-md">
        <q-list separator>
          <q-item clickable v-for="product in state.products" :key="product.id">
            <q-item-section avatar>
              <q-avatar style="height: 125px; width: 90px" square>
                <img :src="`/img/${product.graphicName}`" />
              </q-avatar>
            </q-item-section>
            <q-item-section class="text-left">
              {{ product.productName }}
            </q-item-section>
          </q-item>
        </q-list>
      </q-card>
    </q-scroll-area>
  </div>
</template>

<script>
import { reactive, onMounted } from "vue";
import { fetcher } from "../utils/apiutil";

export default {
  setup() {
    onMounted(() => {
      loadBrands();
    });

    let state = reactive({
      status: "",
      brands: [],
      products: [],
      selectedBrand: {},
      selectedBrandId: "",
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

    const getProducts = async () => {
      try {
        state.selectedBrand = state.brands.find(
          (brand) => brand.id === state.selectedBrandId
        );
        state.status = `finding products for brand ${state.selectedBrand}...`;
        state.products = await fetcher(`Product/${state.selectedBrand.id}`);
        state.status = `loaded ${state.products.length} products for ${state.selectedBrand.name}`;
      } catch (err) {
        console.log(err);
        state.status = `Error has occured: ${err.message}`;
      }
    };

    return {
      state,
      loadBrands,
      getProducts,
    };
  },
};
</script>
