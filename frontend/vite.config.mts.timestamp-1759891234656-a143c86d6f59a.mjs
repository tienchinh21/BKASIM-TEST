// vite.config.mts
import { defineConfig } from "file:///c:/Users/Admin/Desktop/incom-core-test/frontend/node_modules/vite/dist/node/index.js";
import reactRefresh from "file:///c:/Users/Admin/Desktop/incom-core-test/frontend/node_modules/@vitejs/plugin-react/dist/index.js";
import zaloMiniApp from "file:///c:/Users/Admin/Desktop/incom-core-test/frontend/node_modules/zmp-vite-plugin/dist/index.mjs";
var vite_config_default = () => {
  return defineConfig({
    root: "./src",
    base: "",
    plugins: [
      zaloMiniApp(),
      reactRefresh(),
      {
        name: "override-config",
        config: () => ({
          build: {
            target: "esnext"
          }
        })
      }
    ]
  });
};
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcubXRzIl0sCiAgInNvdXJjZXNDb250ZW50IjogWyJjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZGlybmFtZSA9IFwiYzpcXFxcVXNlcnNcXFxcQWRtaW5cXFxcRGVza3RvcFxcXFxpbmNvbS1jb3JlLXRlc3RcXFxcZnJvbnRlbmRcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZmlsZW5hbWUgPSBcImM6XFxcXFVzZXJzXFxcXEFkbWluXFxcXERlc2t0b3BcXFxcaW5jb20tY29yZS10ZXN0XFxcXGZyb250ZW5kXFxcXHZpdGUuY29uZmlnLm10c1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vYzovVXNlcnMvQWRtaW4vRGVza3RvcC9pbmNvbS1jb3JlLXRlc3QvZnJvbnRlbmQvdml0ZS5jb25maWcubXRzXCI7aW1wb3J0IHsgZGVmaW5lQ29uZmlnIH0gZnJvbSBcInZpdGVcIjtcclxuaW1wb3J0IHJlYWN0UmVmcmVzaCBmcm9tIFwiQHZpdGVqcy9wbHVnaW4tcmVhY3RcIjtcclxuaW1wb3J0IHphbG9NaW5pQXBwIGZyb20gXCJ6bXAtdml0ZS1wbHVnaW5cIjtcclxuXHJcbi8vIGh0dHBzOi8vdml0ZWpzLmRldi9jb25maWcvXHJcbmV4cG9ydCBkZWZhdWx0ICgpID0+IHtcclxuICByZXR1cm4gZGVmaW5lQ29uZmlnKHtcclxuICAgIHJvb3Q6IFwiLi9zcmNcIixcclxuICAgIGJhc2U6IFwiXCIsXHJcbiAgICBwbHVnaW5zOiBbXHJcbiAgICAgIHphbG9NaW5pQXBwKCksXHJcbiAgICAgIHJlYWN0UmVmcmVzaCgpLFxyXG4gICAgICB7XHJcbiAgICAgICAgbmFtZTogXCJvdmVycmlkZS1jb25maWdcIixcclxuICAgICAgICBjb25maWc6ICgpID0+ICh7XHJcbiAgICAgICAgICBidWlsZDoge1xyXG4gICAgICAgICAgICB0YXJnZXQ6IFwiZXNuZXh0XCIsXHJcbiAgICAgICAgICB9LFxyXG4gICAgICAgIH0pLFxyXG4gICAgICB9LFxyXG4gICAgXSxcclxuICB9KTtcclxufTtcclxuIl0sCiAgIm1hcHBpbmdzIjogIjtBQUE2VSxTQUFTLG9CQUFvQjtBQUMxVyxPQUFPLGtCQUFrQjtBQUN6QixPQUFPLGlCQUFpQjtBQUd4QixJQUFPLHNCQUFRLE1BQU07QUFDbkIsU0FBTyxhQUFhO0FBQUEsSUFDbEIsTUFBTTtBQUFBLElBQ04sTUFBTTtBQUFBLElBQ04sU0FBUztBQUFBLE1BQ1AsWUFBWTtBQUFBLE1BQ1osYUFBYTtBQUFBLE1BQ2I7QUFBQSxRQUNFLE1BQU07QUFBQSxRQUNOLFFBQVEsT0FBTztBQUFBLFVBQ2IsT0FBTztBQUFBLFlBQ0wsUUFBUTtBQUFBLFVBQ1Y7QUFBQSxRQUNGO0FBQUEsTUFDRjtBQUFBLElBQ0Y7QUFBQSxFQUNGLENBQUM7QUFDSDsiLAogICJuYW1lcyI6IFtdCn0K
