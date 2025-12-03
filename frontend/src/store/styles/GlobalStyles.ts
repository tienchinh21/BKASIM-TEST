const Spacing = {
  none: 0,
  xs: 5,
  sm: 10,
  md: 15,
  lg: 20,
  xl: 30,
  xxl: 40,
  section: 60,
  screen: 80,
};

const Padding = {
  none: 0,
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
};

const BorderRadius = {
  none: 0,
  xs: 5,
  sm: 10,
  md: 15,
  lg: 20,
  xl: 25,
  full: 9999,
  circle: "50%",
};

const FontSize = {
  xs: 10,
  sm: 12,
  base: 14,
  md: 16,
  lg: 18,
  xl: 20,
  xxl: 24,
  title: 30,
};

const FontWeight = {
  thin: 100,
  light: 300,
  regular: 400,
  medium: 500,
  bold: 700,
  black: 900,
};

const getPadding = (v: number, h?: number): React.CSSProperties => ({
  paddingTop: v,
  paddingBottom: v,
  paddingLeft: h ?? v,
  paddingRight: h ?? v,
});

const getMargin = (v: number, h?: number): React.CSSProperties => ({
  marginTop: v,
  marginBottom: v,
  marginLeft: h ?? v,
  marginRight: h ?? v,
});

export const GlobalStyles = {
  spacing: Spacing,
  fontSize: FontSize,
  fontWeight: FontWeight,
  borderRadius: BorderRadius,
  padding: Padding,
  getPadding,
  getMargin,
};
