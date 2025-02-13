import { Validators } from '@angular/forms';
import { domain } from 'ngx-focus-entities';
export const DO_ID = domain({
  htmlType: 'number',
  type: 'number',
});

export const DO_LIBELLE = domain({
  htmlType: 'number',
  type: 'number',
  validators: [Validators.maxLength(100)],
});

export const DO_ENTIER = domain({
  htmlType: 'number',
  type: 'number',
});

export const DO_DATE_HEURE = domain({
  htmlType: 'date',
  type: 'string',
});

export const DO_CODE_LISTE = domain({
  htmlType: 'text',
  type: 'string',
});

export const DO_CODE = domain({
  htmlType: 'text',
  type: 'string',
});

export const DO_EMAIL = domain({
  htmlType: 'email',
  type: 'string',
  validators: [Validators.email],
});

export const DO_BOOLEEN = domain({
  htmlType: 'checkbox',
  type: 'boolean',
});

export const DO_DATE = domain({
  htmlType: 'date',
  type: 'string',
});
